import { Routes, Route } from 'react-router-dom';
import { Suspense, lazy } from 'react';
import Navbar from './components/layout/Navbar';
import Footer from './components/layout/Footer';
import CartDrawer from './components/cart/CartDrawer';
import PageLoader from './components/ui/PageLoader';
import ErrorBoundary from './components/ui/ErrorBoundary';
import './styles/global.css';

// Code-split pages 
const HomePage          = lazy(() => import('./pages/Home/HomePage'));
const ProductsPage      = lazy(() => import('./pages/Products/ProductsPage'));
const ProductDetailPage = lazy(() => import('./pages/Products/ProductDetailPage'));
const BlogPage          = lazy(() => import('./pages/Blog/BlogPage'));
const BlogDetailPage    = lazy(() => import('./pages/Blog/BlogDetailPage'));
const AboutPage         = lazy(() => import('./pages/About/AboutPage'));
const ContactPage       = lazy(() => import('./pages/Contact/ContactPage'));
const CheckoutPage      = lazy(() => import('./pages/Checkout/CheckoutPage'));
const LoginPage         = lazy(() => import('./pages/Auth/LoginPage'));
const RegisterPage      = lazy(() => import('./pages/Auth/RegisterPage'));
const ErrorPage         = lazy(() => import('./pages/Error/ErrorPage'));

function NotFoundPage(): JSX.Element {
  return <ErrorPage code={404} />;
}

export default function App(): JSX.Element {
  return (
    <>
      <Navbar />
      <ErrorBoundary>
        <Suspense fallback={<PageLoader />}>
          <Routes>
            <Route path="/"               element={<HomePage />} />
            <Route path="/products"       element={<ProductsPage />} />
            <Route path="/products/:slug" element={<ProductDetailPage />} />
            <Route path="/blog"           element={<BlogPage />} />
            <Route path="/blog/:id"       element={<BlogDetailPage />} />
            <Route path="/about"          element={<AboutPage />} />
            <Route path="/contact"        element={<ContactPage />} />
            <Route path="/checkout"       element={<CheckoutPage />} />
            <Route path="/login"          element={<LoginPage />} />
            <Route path="/register"       element={<RegisterPage />} />
            <Route path="/error"          element={<ErrorPage />} />
            <Route path="*"               element={<NotFoundPage />} />
          </Routes>
        </Suspense>
      </ErrorBoundary>
      <Footer />
      <CartDrawer />
    </>
  );
}
