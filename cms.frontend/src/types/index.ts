// Domain types

export interface Category {
  id: number;
  name: string;
  slug: string;
  description: string;
  icon: string;
}

export interface Product {
  id: number;
  slug: string;
  name: string;
  description: string;
  price: number;
  originalPrice: number | null;
  discount?: number;
  rating: number;
  reviewCount?: number;
  stockQuantity: number;
  imageUrl: string;
  categoryProductId: number;
  tags: string[];
  badge: string | null;
  gallery?: string[];
  colors?: string[];
  sizes?: string[];
}

export interface Post {
  id: number;
  slug: string;
  title: string;
  content: string;
  excerpt?: string;
  imageUrl: string;
  createdDate: string;
  categoryId: number;
  category: { id: number; name: string };
}

export interface TeamMember {
  id: number;
  name: string;
  role: string;
  imageUrl: string;
  social: {
    facebook?: string;
    instagram?: string;
    twitter?: string;
  };
}

// Cart types

export interface CartItem {
  id: number;
  slug: string;
  name: string;
  price: number;
  imageUrl: string;
  quantity: number;
  size?: string;
  color?: string;
}

//UI types

export interface BreadcrumbItem {
  label: string;
  href?: string;
}

export interface SEOOptions {
  title?: string;
  description?: string;
  image?: string;
  url?: string;
  type?: string;
  structuredData?: object | null;
}

export type SortOption = 'default' | 'price_asc' | 'price_desc' | 'name_asc' | 'rating_desc';
export type ViewMode   = 'grid' | 'list';
export type SubmitStatus = '' | 'sending' | 'success' | 'error';
