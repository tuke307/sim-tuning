import { Spinner } from '@sim-tuning/ui/components/ui/spinner';

export default function RootLoading() {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="text-center">
        <div className="w-20 h-20 mx-auto">
          <Spinner className="size-8 text-primary" />
        </div>
      </div>
    </div>
  );
}
