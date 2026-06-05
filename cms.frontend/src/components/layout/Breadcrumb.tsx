import { Link } from 'react-router-dom';
import type { BreadcrumbItem } from '../../types';

const css = `
.bc{display:flex;align-items:center;gap:8px;font-size:14px;color:rgba(0,0,0,.6);flex-wrap:wrap;padding:0}
.bc a{color:rgba(0,0,0,.6);text-decoration:none;transition:color .2s}
.bc a:hover{color:#000}
.bc-sep{color:rgba(0,0,0,.3)}
.bc-current{color:#000;font-weight:500}
`;

interface BreadcrumbProps {
  items: BreadcrumbItem[];
  className?: string;
}

export default function Breadcrumb({ items, className = '' }: BreadcrumbProps): JSX.Element {
  return (
    <>
      <style>{css}</style>
      <nav aria-label="Breadcrumb">
        <ol className={`bc ${className}`}>
          {items.map((item, i) => (
            <li key={i} style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              {i > 0 && <span className="bc-sep" aria-hidden="true">/</span>}
              {item.href && i < items.length - 1 ? (
                <Link to={item.href}>{item.label}</Link>
              ) : (
                <span className={i === items.length - 1 ? 'bc-current' : ''} aria-current={i === items.length - 1 ? 'page' : undefined}>
                  {item.label}
                </span>
              )}
            </li>
          ))}
        </ol>
      </nav>
    </>
  );
}
