import React, { useState } from 'react';
import styles from './Newsletter.module.css';

const Newsletter: React.FC = () => {
  const [email, setEmail] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (email) {
      alert(`Đã đăng ký! Chúng tôi sẽ gửi ưu đãi tới ${email}`);
      setEmail('');
    }
  };

  return (
    <section className={styles.wrapper}>
      <div className={styles.box}>
        <h2 className={styles.title}>
          ĐĂNG KÝ NHẬN ƯU ĐÃI<br />MỚI NHẤT TỪ CHÚNG TÔI
        </h2>
        <form className={styles.form} onSubmit={handleSubmit}>
          <div className={styles.inputRow}>
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
              <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" stroke="rgba(0,0,0,0.4)" strokeWidth="1.5"/>
              <polyline points="22,6 12,13 2,6" stroke="rgba(0,0,0,0.4)" strokeWidth="1.5"/>
            </svg>
            <input
              type="email"
              placeholder="Nhập địa chỉ email của bạn"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className={styles.input}
              required
            />
          </div>
          <button type="submit" className={styles.btn}>Đăng ký nhận tin</button>
        </form>
      </div>
    </section>
  );
};

export default Newsletter;
