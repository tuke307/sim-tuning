import { paginationSchema } from './pagination.util';
import { orderSchema } from './order.util';
import { searchSchema } from './search.util';
import { createIncludeSchema } from './include.util';

export const getAllSchema = (validIncludes: string[] = []) => {
  return paginationSchema
    .merge(orderSchema)
    .merge(searchSchema)
    .merge(createIncludeSchema(validIncludes));
};
