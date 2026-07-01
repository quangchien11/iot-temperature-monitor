import { theme } from 'antd';

// Theme tổng thể cho toàn bộ dashboard. Tông xanh chủ đạo, bo góc mềm,
// card có đổ bóng nhẹ tạo cảm giác hiện đại.
export const themeConfig = {
  algorithm: theme.defaultAlgorithm,
  token: {
    colorPrimary: '#1677ff',
    borderRadius: 12,
    fontFamily:
      "'Segoe UI', -apple-system, 'Helvetica Neue', Roboto, Arial, sans-serif",
    colorBgLayout: '#f0f4fb',
  },
  components: {
    Card: {
      boxShadowTertiary:
        '0 4px 16px rgba(15, 34, 67, 0.06), 0 1px 4px rgba(15, 34, 67, 0.04)',
    },
    Statistic: {
      contentFontSize: 30,
    },
  },
};
