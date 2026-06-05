const css = `
.shopco-loader{display:flex;align-items:center;justify-content:center;min-height:60vh}
.shopco-spinner{width:40px;height:40px;border:3px solid #f0f0f0;border-top-color:#000;border-radius:50%;animation:shopco-spin 0.7s linear infinite}
@keyframes shopco-spin{to{transform:rotate(360deg)}}
`;

export default function PageLoader(): JSX.Element {
  return (
    <>
      <style>{css}</style>
      <div className="shopco-loader">
        <div className="shopco-spinner" />
      </div>
    </>
  );
}
