//Ho ten: Quang Văn Trường || MSV: 2123170591
//Môn: ASP.NET Core || Giảng viên: Nguyễn Cao Thái
// BUỔI 7 – submitContact POST dữ liệu form liên hệ lên Backend
import { useState, type FormEvent } from 'react';
import { submitContact } from '../../services/contactService';
import { useSEO } from '../../seo';
import Breadcrumb from '../../components/layout/Breadcrumb';
import './ContactPage.css';

export default function ContactPage(): JSX.Element {
  useSEO({ title: 'Liên hệ', description: 'Liên hệ với SHOP.CO – chúng tôi luôn sẵn lòng lắng nghe bạn.' });

  const [form, setForm]     = useState({ name: '', email: '', message: '' });
  const [status, setStatus] = useState<'' | 'sending' | 'success' | 'error'>('');

  const handleField = (field: string, value: string) => setForm((f) => ({ ...f, [field]: value }));

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setStatus('sending');
    try {
      await submitContact(form.name, form.email, form.message);
      setStatus('success');
      setForm({ name: '', email: '', message: '' });
    } catch {
      setStatus('error');
    }
  };

  return (
    <div className="page-content contact-page">
      <div className="container">
        <div className="contact-breadcrumb">
          <Breadcrumb items={[{ label: 'Trang chủ', href: '/' }, { label: 'Liên hệ' }]} />
        </div>

        <div className="contact-layout">
          {/* Info */}
          <div className="contact-info">
            <h1 className="contact-title">Liên hệ với chúng tôi</h1>
            <p className="contact-sub">Bạn có câu hỏi hay chỉ muốn chào một tiếng? Chúng tôi luôn sẵn lòng lắng nghe.</p>

            <div className="contact-details">
              {[
                { icon: '', label: 'Email', value: 'hello@shop.co' },
                { icon: '', label: 'Điện thoại', value: '+84 28 1234 5678' },
                { icon: '', label: 'Địa chỉ', value: '123 Đường Thời Trang, Quận 1, TP. Hồ Chí Minh' },
              ].map(({ icon, label, value }) => (
                <div key={label} className="contact-detail">
                  <span className="contact-detail-icon">{icon}</span>
                  <div>
                    <p className="contact-detail-label">{label}</p>
                    <p className="contact-detail-value">{value}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Form */}
          <div className="contact-form-wrap">
            <h2 className="contact-form-title">Gửi tin nhắn</h2>

            {status === 'success' ? (
              <div className="contact-success">
                <p>✓ Đã gửi! Chúng tôi sẽ phản hồi trong vòng 24 giờ.</p>
              </div>
            ) : (
              <form onSubmit={handleSubmit} className="contact-form">
                <div className="contact-field">
                  <label htmlFor="c-name">Họ tên</label>
                  <input id="c-name" required value={form.name} onChange={(e) => handleField('name', e.target.value)} placeholder="Nguyễn Văn A" />
                </div>
                <div className="contact-field">
                  <label htmlFor="c-email">Email</label>
                  <input id="c-email" type="email" required value={form.email} onChange={(e) => handleField('email', e.target.value)} placeholder="you@example.com" />
                </div>
                <div className="contact-field">
                  <label htmlFor="c-msg">Nội dung</label>
                  <textarea id="c-msg" rows={5} required value={form.message} onChange={(e) => handleField('message', e.target.value)} placeholder="Chúng tôi có thể giúp gì cho bạn?" />
                </div>
                {status === 'error' && <p className="contact-error">Đã có lỗi xảy ra. Vui lòng thử lại.</p>}
                <button type="submit" className="contact-btn" disabled={status === 'sending'}>
                  {status === 'sending' ? 'Đang gửi…' : 'Gửi tin nhắn'}
                </button>
              </form>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
