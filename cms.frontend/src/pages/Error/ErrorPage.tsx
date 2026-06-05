import { Link } from 'react-router-dom';
import { useSEO } from '../../seo';
import './ErrorPage.css';

interface ErrorPageProps {
  code?: number;
  message?: string;
}

export default function ErrorPage({ code = 500, message }: ErrorPageProps): JSX.Element {
  const is404 = code === 404;

  useSEO({ title: `${code} ${is404 ? 'Không tìm thấy trang' : 'Lỗi máy chủ'}` });

  return (
    <div className="page-content error-page">
      <div className="container error-inner">
        <div className="error-code">{code}</div>
        <h1 className="error-title">
          {is404 ? 'Không tìm thấy trang' : (message ?? 'Đã có lỗi xảy ra')}
        </h1>
        <p className="error-desc">
          {is404
            ? 'Trang bạn tìm không tồn tại hoặc đã được di chuyển.'
            : 'Rất tiếc vì sự bất tiện này. Vui lòng thử lại sau.'}
        </p>
        <Link to="/" className="error-btn">Về trang chủ</Link>
        {!is404 && (
          <button className="error-retry" onClick={() => window.location.reload()}>
            Thử lại
          </button>
        )}
      </div>
    </div>
  );
}
