import { Injectable } from '@nestjs/common';
import { logger } from '@sim-tuning/logger';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import handlebars from 'handlebars';
import mjml2html from 'mjml';
import juice from 'juice';

type TemplateData = Record<string, unknown>;
type HandlebarsTemplate = handlebars.TemplateDelegate<TemplateData>;

type CachedTemplate = {
  compiled: HandlebarsTemplate;
  mtimeMs: number;
};

type MjmlRenderResult = {
  html: string;
  errors: Array<{ message?: string } | string>;
};

const currentDir = path.dirname(fileURLToPath(import.meta.url));
const compiledTemplatesDir = path.resolve(currentDir, '../../../templates');
const sourceTemplatesDir = path.resolve(currentDir, '../../../../templates');
const templatesDir = fs.existsSync(compiledTemplatesDir)
  ? compiledTemplatesDir
  : sourceTemplatesDir;
const allowedExtensions = new Set(['.html', '.mjml']);

@Injectable()
export class EmailTemplateRendererService {
  private readonly templateCache = new Map<string, CachedTemplate>();
  private readonly subjectCache = new Map<string, HandlebarsTemplate>();

  renderSubject(template: string, data: TemplateData): string {
    const compiled = this.getCompiledSubject(template);
    return compiled(data);
  }

  async renderTemplate(templateFile: string, data: TemplateData): Promise<string> {
    const templatePath = this.resolveTemplatePath(templateFile);
    const compiled = this.getCompiledTemplate(templatePath);
    const rendered = compiled(data);
    const extension = path.extname(templatePath).toLowerCase();
    const html = extension === '.mjml' ? await this.renderMjml(rendered, templateFile) : rendered;

    return this.inlineHtml(html);
  }

  private getCompiledSubject(template: string): HandlebarsTemplate {
    const cached = this.subjectCache.get(template);
    if (cached) {
      return cached;
    }

    const compiled = handlebars.compile(template);
    this.subjectCache.set(template, compiled);
    return compiled;
  }

  private getCompiledTemplate(templatePath: string): HandlebarsTemplate {
    const stats = fs.statSync(templatePath);
    const cached = this.templateCache.get(templatePath);

    if (cached && cached.mtimeMs === stats.mtimeMs) {
      return cached.compiled;
    }

    const templateSource = fs.readFileSync(templatePath, 'utf-8');
    const compiled = handlebars.compile(templateSource);
    this.templateCache.set(templatePath, { compiled, mtimeMs: stats.mtimeMs });
    return compiled;
  }

  private resolveTemplatePath(templateFile: string): string {
    if (!templateFile) {
      throw new Error('Template file is required');
    }

    const normalized = templateFile.replace(/\\/g, '/');
    const resolvedPath = path.resolve(templatesDir, normalized);

    if (!resolvedPath.startsWith(`${templatesDir}${path.sep}`)) {
      throw new Error('Invalid template path');
    }

    const extension = path.extname(resolvedPath).toLowerCase();
    if (!allowedExtensions.has(extension)) {
      throw new Error(`Unsupported template extension: ${extension}`);
    }

    if (!fs.existsSync(resolvedPath)) {
      throw new Error(`Template not found: ${templateFile}`);
    }

    return resolvedPath;
  }

  private async renderMjml(mjmlMarkup: string, templateFile: string): Promise<string> {
    const result = (await mjml2html(mjmlMarkup, { validationLevel: 'soft' })) as MjmlRenderResult;
    const errors = result.errors ?? [];

    if (errors.length > 0) {
      const errorMessages = errors
        .map((error) => (typeof error === 'string' ? error : error.message))
        .filter(Boolean)
        .join(' | ');

      logger.error(`MJML render errors for ${templateFile}: ${errorMessages}`);
    }

    if (!result.html) {
      throw new Error(`MJML render failed for ${templateFile}`);
    }

    return result.html;
  }

  private inlineHtml(html: string): string {
    return juice(html, {
      preserveImportant: true,
      applyStyleTags: true,
      removeStyleTags: true,
      applyWidthAttributes: true,
    });
  }
}
