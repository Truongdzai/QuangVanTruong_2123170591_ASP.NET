import { Component, type ReactNode, type ErrorInfo } from 'react';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

const css = `
.shopco-error-boundary{display:flex;flex-direction:column;align-items:center;justify-content:center;min-height:40vh;padding:2rem;text-align:center}
.shopco-error-boundary h2{font-size:1.5rem;font-weight:700;margin-bottom:1rem}
.shopco-error-boundary p{color:#666;margin-bottom:1.5rem}
.shopco-error-boundary button{padding:0.75rem 2rem;background:#000;color:#fff;border:none;border-radius:62px;font-size:0.95rem;cursor:pointer}
`;

export default class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false, error: null };

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, info: ErrorInfo): void {
    console.error('ErrorBoundary caught:', error, info);
  }

  render(): ReactNode {
    if (this.state.hasError) {
      if (this.props.fallback) return this.props.fallback;
      return (
        <>
          <style>{css}</style>
          <div className="shopco-error-boundary">
            <h2>Đã có lỗi xảy ra</h2>
            <p>{this.state.error?.message ?? 'Đã xảy ra lỗi không mong muốn.'}</p>
            <button onClick={() => this.setState({ hasError: false, error: null })}>
              Thử lại
            </button>
          </div>
        </>
      );
    }
    return this.props.children;
  }
}
