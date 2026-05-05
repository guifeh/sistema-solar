import { type InputHTMLAttributes, forwardRef } from 'react';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  icon?: React.ReactNode;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, icon, className = '', id, ...props }, ref) => {
    const inputId = id || label?.toLowerCase().replace(/\s+/g, '-');

    return (
      <div className="flex flex-col gap-2">
        {label && (
          <label
            htmlFor={inputId}
            className="block text-sm font-semibold tracking-wide text-surface-300 uppercase ml-1"
          >
            {label}
          </label>
        )}
        <div className="relative flex items-center">
          {icon && (
            <div className="absolute left-4 flex items-center pointer-events-none text-surface-500">
              {icon}
            </div>
          )}
          <input
            ref={ref}
            id={inputId}
            className={`
              w-full rounded-2xl bg-surface-900 border-2 border-surface-800
              text-surface-100 placeholder-surface-600
              focus:outline-none focus:ring-4 focus:ring-solar-500/20 focus:border-solar-500
              transition-all duration-300 ease-out
              pr-5 py-4 text-lg
              ${error ? 'border-red-500 focus:ring-red-500/30 focus:border-red-500' : ''}
              ${className}
            `}
            style={{ paddingLeft: icon ? '3.5rem' : '1.25rem' }}
            {...props}
          />
        </div>
        {error && (
          <p className="text-sm text-red-400 mt-1">{error}</p>
        )}
      </div>
    );
  }
);

Input.displayName = 'Input';
