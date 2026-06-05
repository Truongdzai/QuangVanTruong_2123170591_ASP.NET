import {
  createContext,
  useContext,
  useReducer,
  useCallback,
  useMemo,
  useEffect,
  type ReactNode,
} from 'react';
import type { CartItem, Product } from '../types';

// State & Actions

interface CartState {
  items: CartItem[];
  isOpen: boolean;
}

type CartAction =
  | { type: 'ADD';      payload: CartItem }
  | { type: 'REMOVE';   payload: { id: number } }
  | { type: 'UPDATE';   payload: { id: number; quantity: number } }
  | { type: 'CLEAR' }
  | { type: 'OPEN' }
  | { type: 'CLOSE' }
  | { type: 'TOGGLE' };

function cartReducer(state: CartState, action: CartAction): CartState {
  switch (action.type) {
    case 'ADD': {
      const exists = state.items.find((i) => i.id === action.payload.id);
      if (exists) {
        return {
          ...state,
          items: state.items.map((i) =>
            i.id === action.payload.id
              ? { ...i, quantity: i.quantity + action.payload.quantity }
              : i,
          ),
        };
      }
      return { ...state, items: [...state.items, action.payload] };
    }
    case 'REMOVE':
      return { ...state, items: state.items.filter((i) => i.id !== action.payload.id) };
    case 'UPDATE':
      return {
        ...state,
        items: state.items
          .map((i) => i.id === action.payload.id ? { ...i, quantity: action.payload.quantity } : i)
          .filter((i) => i.quantity > 0),
      };
    case 'CLEAR':  return { ...state, items: [] };
    case 'OPEN':   return { ...state, isOpen: true };
    case 'CLOSE':  return { ...state, isOpen: false };
    case 'TOGGLE': return { ...state, isOpen: !state.isOpen };
    default:       return state;
  }
}

//Context

interface CartContextValue {
  items:       CartItem[];
  isOpen:      boolean;
  totalItems:  number;
  totalPrice:  number;
  addToCart:   (product: Product, qty?: number) => void;
  removeItem:  (id: number) => void;
  updateQty:   (id: number, quantity: number) => void;
  clearCart:   () => void;
  openCart:    () => void;
  closeCart:   () => void;
  toggleCart:  () => void;
}

const CartContext = createContext<CartContextValue | null>(null);

const STORAGE_KEY = 'shopco_cart';

function loadFromStorage(): CartItem[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    return JSON.parse(raw) as CartItem[];
  } catch {
    return [];
  }
}

//Provider

export function CartProvider({ children }: { children: ReactNode }): JSX.Element {
  const [state, dispatch] = useReducer(cartReducer, {
    items: loadFromStorage(),
    isOpen: false,
  });

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state.items));
  }, [state.items]);

  const addToCart = useCallback((product: Product, qty = 1): void => {
    dispatch({
      type: 'ADD',
      payload: {
        id:       product.id,
        slug:     product.slug,
        name:     product.name,
        price:    product.price,
        imageUrl: product.imageUrl,
        quantity: qty,
      },
    });
  }, []);

  const removeItem  = useCallback((id: number) => dispatch({ type: 'REMOVE',  payload: { id } }), []);
  const updateQty   = useCallback((id: number, quantity: number) => dispatch({ type: 'UPDATE', payload: { id, quantity } }), []);
  const clearCart   = useCallback(() => dispatch({ type: 'CLEAR' }), []);
  const openCart    = useCallback(() => dispatch({ type: 'OPEN' }), []);
  const closeCart   = useCallback(() => dispatch({ type: 'CLOSE' }), []);
  const toggleCart  = useCallback(() => dispatch({ type: 'TOGGLE' }), []);

  const totalItems = useMemo(() => state.items.reduce((s, i) => s + i.quantity, 0), [state.items]);
  const totalPrice = useMemo(() => state.items.reduce((s, i) => s + i.price * i.quantity, 0), [state.items]);

  const value = useMemo<CartContextValue>(() => ({
    items: state.items, isOpen: state.isOpen,
    totalItems, totalPrice,
    addToCart, removeItem, updateQty, clearCart,
    openCart, closeCart, toggleCart,
  }), [state.items, state.isOpen, totalItems, totalPrice,
       addToCart, removeItem, updateQty, clearCart, openCart, closeCart, toggleCart]);

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart(): CartContextValue {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error('useCart must be used inside <CartProvider>');
  return ctx;
}
