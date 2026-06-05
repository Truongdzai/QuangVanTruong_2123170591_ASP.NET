import { useEffect } from 'react';
import type { SEOOptions } from '../types';
import { ENV } from '../configs/env';

/**
 * useSEO — set page-level <title>, <meta>, and structured data in <head>.
 * Called once per page component with the page's SEO options.
 */
export default function useSEO(options: SEOOptions = {}): void {
  const {
    title,
    description = 'Chúng tôi có những trang phục hợp phong cách của bạn và khiến bạn tự hào khi khoác lên mình. Từ nữ đến nam.',
    image,
    url,
    type = 'website',
    structuredData,
  } = options;

  useEffect(() => {
    const fullTitle = title ? `${title} | ${ENV.SITE_NAME}` : `${ENV.SITE_NAME} – Tìm trang phục hợp phong cách của bạn`;
    document.title = fullTitle;

    setMeta('description', description);
    setOg('title', fullTitle);
    setOg('description', description);
    setOg('type', type);
    if (url)   setOg('url', url);
    if (image) setOg('image', image);

    if (structuredData) {
      let el = document.getElementById('ld-json') as HTMLScriptElement | null;
      if (!el) {
        el = document.createElement('script');
        el.id   = 'ld-json';
        el.type = 'application/ld+json';
        document.head.appendChild(el);
      }
      el.textContent = JSON.stringify(structuredData);
    }

    return () => {
      // Reset to defaults on unmount to avoid stale tags
      document.title = `${ENV.SITE_NAME} – Tìm trang phục hợp phong cách của bạn`;
    };
  }, [title, description, image, url, type, structuredData]);
}

function setMeta(name: string, content: string): void {
  let el = document.querySelector<HTMLMetaElement>(`meta[name="${name}"]`);
  if (!el) {
    el = document.createElement('meta');
    el.setAttribute('name', name);
    document.head.appendChild(el);
  }
  el.setAttribute('content', content);
}

function setOg(property: string, content: string): void {
  let el = document.querySelector<HTMLMetaElement>(`meta[property="og:${property}"]`);
  if (!el) {
    el = document.createElement('meta');
    el.setAttribute('property', `og:${property}`);
    document.head.appendChild(el);
  }
  el.setAttribute('content', content);
}
