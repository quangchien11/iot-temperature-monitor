// Lớp gọi API ASP.NET Core.
// - Mặc định BASE = '' → dùng đường dẫn tương đối (cùng origin). Hợp cho 2 trường hợp:
//     + Dev: `npm run dev`, Vite proxy /api về http://localhost:5094 (xem vite.config.js).
//     + Prod chung origin: web build vào wwwroot của API.
// - Muốn chạy FE hoàn toàn độc lập, trỏ tới API ở host khác: đặt biến
//   VITE_API_BASE (ví dụ trong web/.env.local) = http://localhost:5094
//   thì mọi request sẽ gọi tới đó. Xem web/.env.example.
const BASE = import.meta.env.VITE_API_BASE ?? '';

async function getJson(url) {
  const res = await fetch(BASE + url);
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

export const api = {
  getDashboard: () => getJson('/api/temperature/dashboard'),
  getStatistics: (period) => getJson(`/api/temperature/${period}`),
  getHistory: (count = 50) => getJson(`/api/temperature/history?count=${count}`),
  getAlertSetting: () => getJson('/api/alert'),
  getAlertLogs: (count = 15) => getJson(`/api/alert/logs?count=${count}`),

  async updateAlertSetting(dto) {
    const res = await fetch(BASE + '/api/alert', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto),
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },
};
