/// <reference types="vite/client" />

export {};

declare global {
  interface ImportMetaEnv {
    readonly VITE_API_BASE_URL: string;
    readonly VITE_SITE_URL: string;
    readonly VITE_SITE_NAME: string;
    readonly VITE_USE_MOCK: string;
  }

  interface ImportMeta {
    readonly env: ImportMetaEnv;
  }

  /** Re-expose React.JSX as the global JSX namespace (React 19 / @types/react 19) */
  namespace JSX {
    type Element = import('react').JSX.Element;
    type IntrinsicElements = import('react').JSX.IntrinsicElements;
    type ElementChildrenAttribute = import('react').JSX.ElementChildrenAttribute;
    type LibraryManagedAttributes<C, P> = import('react').JSX.LibraryManagedAttributes<C, P>;
    type IntrinsicAttributes = import('react').JSX.IntrinsicAttributes;
    type IntrinsicClassAttributes<T> = import('react').JSX.IntrinsicClassAttributes<T>;
    type ElementType = import('react').JSX.ElementType;
  }
}
