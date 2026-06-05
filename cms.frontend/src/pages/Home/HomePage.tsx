//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// BUỔI 7 – useEffect gọi getProducts() (1 request) hiển thị sản phẩm từ Backend
import { useEffect, useState } from 'react';
import { getProducts } from '../../services/productService';
import type { Product } from '../../types';
import { useSEO } from '../../seo';
import HeroSection from '../../components/sections/HeroSection';
import BrandsStrip from '../../components/sections/BrandsStrip';
import ProductSection from '../../components/sections/ProductSection';
import StyleSection from '../../components/sections/StyleSection';
import Testimonials from '../../components/sections/Testimonials';
import NewsSection from '../../components/sections/NewsSection';
import Newsletter from '../../components/sections/Newsletter';

export default function HomePage(): JSX.Element {
  useSEO({
    title: 'Tìm trang phục hợp phong cách của bạn',
    description: 'SHOP.CO – Khám phá hàng mới về, sản phẩm bán chạy và nhiều hơn nữa.',
  });

  // Buổi 7: 1 request GET /products — 4 sản phẩm đầu là "New Arrivals", 4 kế tiếp là "Top Selling"
  const [products, setProducts] = useState<Product[]>([]);

  useEffect(() => {
    getProducts().then(setProducts).catch(console.error);
  }, []);

  return (
    <main>
      <HeroSection />
      <BrandsStrip />
      <ProductSection title="HÀNG MỚI VỀ" products={products.slice(0, 4)} sectionId="new-arrivals" />
      <ProductSection title="BÁN CHẠY NHẤT" products={products.slice(4, 8)} sectionId="top-selling" />
      <StyleSection />
      <Testimonials />
      <NewsSection />
      <Newsletter />
    </main>
  );
}
