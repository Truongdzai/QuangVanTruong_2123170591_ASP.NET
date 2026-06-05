import { useSEO } from '../../seo';
import Breadcrumb from '../../components/layout/Breadcrumb';
import './AboutPage.css';

export default function AboutPage(): JSX.Element {
  useSEO({ title: 'Giới thiệu', description: 'Tìm hiểu về SHOP.CO — câu chuyện, giá trị và những con người phía sau thương hiệu.' });

  return (
    <div className="page-content about-page">
      <div className="container">
        <div className="about-breadcrumb">
          <Breadcrumb items={[{ label: 'Trang chủ', href: '/' }, { label: 'Giới thiệu' }]} />
        </div>

        {/* Hero */}
        <section className="about-hero">
          <h1 className="about-hero-title">Chúng tôi là SHOP.CO</h1>
          <p className="about-hero-sub">
            Tìm trang phục hợp phong cách của bạn. Từ nữ đến nam, chúng tôi có mọi thứ bạn cần để trông thật đẹp và tự tin.
          </p>
        </section>

        {/* Stats */}
        <section className="about-stats">
          {[
            { value: '200+', label: 'Thương hiệu quốc tế' },
            { value: '2,000+', label: 'Sản phẩm chất lượng cao' },
            { value: '30,000+', label: 'Khách hàng hài lòng' },
          ].map(({ value, label }) => (
            <div key={label} className="about-stat">
              <strong>{value}</strong>
              <span>{label}</span>
            </div>
          ))}
        </section>

        {/* Mission */}
        <section className="about-section">
          <h2>Sứ mệnh của chúng tôi</h2>
          <p>
            Tại SHOP.CO, chúng tôi tin rằng phong cách đẹp không nhất thiết phải phức tạp hay đắt đỏ. Chúng tôi tuyển chọn những trang phục tốt nhất từ khắp nơi, mang đến các phong cách phù hợp cho mọi dịp — từ cuối tuần thường ngày, sự kiện trang trọng, buổi tập gym đến mọi khoảnh khắc khác.
          </p>
        </section>

        {/* Values */}
        <section className="about-section">
          <h2>Giá trị của chúng tôi</h2>
          <div className="about-values">
            {[
              { icon: '', title: 'Bền vững', desc: 'Chúng tôi hợp tác với các thương hiệu cam kết sản xuất có đạo đức và dùng chất liệu bền vững.' },
              { icon: '', title: 'Chất lượng', desc: 'Mỗi sản phẩm đều được kiểm duyệt kỹ lưỡng để đảm bảo đạt tiêu chuẩn cao của chúng tôi.' },
              { icon: '', title: 'Cộng đồng', desc: 'Chúng tôi đồng hành cùng khách hàng và cộng đồng nơi các đối tác hoạt động.' },
              { icon: '', title: 'Đổi mới', desc: 'Chúng tôi không ngừng làm mới bộ sưu tập để luôn dẫn đầu xu hướng.' },
            ].map(({ icon, title, desc }) => (
              <div key={title} className="about-value-card">
                <span className="about-value-icon">{icon}</span>
                <h3>{title}</h3>
                <p>{desc}</p>
              </div>
            ))}
          </div>
        </section>
      </div>
    </div>
  );
}
