import type { CartItem } from '../../types';

export function calcSubtotal(items: CartItem[]): number {
  return items.reduce((sum, item) => sum + item.price * item.quantity, 0);
}

export function calcShipping(_subtotal: number): number {
  // Free shipping for orders over $100
  return _subtotal >= 100 ? 0 : 9.99;
}

export function calcTotal(items: CartItem[]): number {
  const subtotal = calcSubtotal(items);
  return subtotal + calcShipping(subtotal);
}

export function formatOrderId(): string {
  return `ORD-${Date.now().toString(36).toUpperCase()}`;
}
