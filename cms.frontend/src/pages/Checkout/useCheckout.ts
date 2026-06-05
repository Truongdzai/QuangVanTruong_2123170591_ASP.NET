import { useState } from 'react';
import { useCart } from '../../contexts/CartContext';
import { calcSubtotal, calcShipping, calcTotal, formatOrderId } from './checkout.utils';

export type CheckoutStep = 'shipping' | 'payment' | 'confirmation';

export interface ShippingForm {
  firstName: string;
  lastName:  string;
  email:     string;
  address:   string;
  city:      string;
  zip:       string;
  country:   string;
}

const INITIAL_FORM: ShippingForm = {
  firstName: '',
  lastName:  '',
  email:     '',
  address:   '',
  city:      '',
  zip:       '',
  country:   'US',
};

export function useCheckout() {
  const { items, totalPrice, clearCart } = useCart();
  const [step,    setStep]    = useState<CheckoutStep>('shipping');
  const [form,    setForm]    = useState<ShippingForm>(INITIAL_FORM);
  const [orderId, setOrderId] = useState('');

  const subtotal = calcSubtotal(items);
  const shipping = calcShipping(subtotal);
  const total    = calcTotal(items);

  const updateField = (field: keyof ShippingForm, value: string) =>
    setForm((f) => ({ ...f, [field]: value }));

  const submitShipping = () => setStep('payment');

  const submitPayment = () => {
    const id = formatOrderId();
    setOrderId(id);
    clearCart();
    setStep('confirmation');
  };

  return {
    step, form, orderId,
    subtotal, shipping, total, items,
    updateField, submitShipping, submitPayment,
    goBack: () => setStep('shipping'),
  };
}
