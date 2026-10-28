import { TrendingDown, TrendingUp } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '../ui/card';
import { Skeleton } from '../ui/skeleton';

interface StatCardProps {
  className?: string;
  title: string;
  value: string | number;
  description: string;
  icon: React.ReactNode;
  loading: boolean;
  /** Optional trend comparison — omitted when the metric has no previous-period value. */
  trend?: {
    value: number;
    isUp: boolean;
  };
}

export const StatCard = ({
  className,
  title,
  value,
  description,
  icon,
  loading,
  trend,
}: StatCardProps) => {
  return (
    <Card className={`${className || ''}`}>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium">{title}</CardTitle>
        {icon}
      </CardHeader>
      <CardContent>
        {loading ? (
          <Skeleton className="h-8 w-[100px] mb-2" />
        ) : (
          <div className="text-2xl font-bold">{value}</div>
        )}
        {loading ? (
          <Skeleton className="h-4 w-[120px]" />
        ) : (
          <p className="text-xs text-muted-foreground flex items-center gap-1">
            {trend && (
              <>
                {trend.isUp ? (
                  <TrendingUp className="h-3 w-3 text-success" />
                ) : (
                  <TrendingDown className="h-3 w-3 text-destructive" />
                )}
                <span
                  className={
                    trend.value === 0
                      ? 'text-muted-foreground'
                      : trend.isUp
                        ? 'text-success'
                        : 'text-destructive'
                  }
                >
                  {trend.value === 0
                    ? 'Keine Änderung'
                    : `${trend.value > 0 ? '+' : ''}${trend.value}%`}
                </span>
              </>
            )}
            <span>{description}</span>
          </p>
        )}
      </CardContent>
    </Card>
  );
};
