import { useCallback, useEffect, useRef, useState } from 'react';
import { Layout, Row, Col, Alert, Space, Tag } from 'antd';
import { ThunderboltFilled } from '@ant-design/icons';
import { api } from './api.js';
import StatCards from './components/StatCards.jsx';
import TempChart from './components/TempChart.jsx';
import HistoryTable from './components/HistoryTable.jsx';
import AlertSettings from './components/AlertSettings.jsx';
import AlertLogs from './components/AlertLogs.jsx';

const { Header, Content } = Layout;
const REFRESH_MS = 5000;

export default function App() {
  const [dashboard, setDashboard] = useState(null);
  const [history, setHistory] = useState([]);
  const [logs, setLogs] = useState([]);
  const [online, setOnline] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);

  const refresh = useCallback(async () => {
    try {
      const [d, h, l] = await Promise.all([
        api.getDashboard(),
        api.getHistory(40),
        api.getAlertLogs(15),
      ]);
      setDashboard(d);
      setHistory(h);
      setLogs(l);
      setOnline(true);
    } catch {
      setOnline(false);
    }
    setRefreshKey((k) => k + 1);
  }, []);

  // Dùng ref để interval luôn gọi bản refresh mới nhất.
  const refreshRef = useRef(refresh);
  refreshRef.current = refresh;

  useEffect(() => {
    refreshRef.current();
    const id = setInterval(() => refreshRef.current(), REFRESH_MS);
    return () => clearInterval(id);
  }, []);

  const isAlert = !!dashboard?.isAlert;

  return (
    <Layout style={{ background: 'transparent', minHeight: '100vh' }}>
      <Header className="app-header" style={{ padding: '0 24px' }}>
        <Row align="middle" justify="space-between" style={{ height: '100%' }}>
          <Col>
            <span className="app-brand">
              <span className="brand-icon">
                <ThunderboltFilled />
              </span>
              Giám sát &amp; Cảnh báo Nhiệt độ
            </span>
          </Col>
          <Col>
            <Tag
              color={online ? 'success' : 'error'}
              style={{ fontSize: 13, padding: '4px 12px', borderRadius: 20, margin: 0 }}
            >
              <span className={`pulse-dot ${online ? 'online' : 'offline'}`} />
              {online ? 'Đã kết nối' : 'Mất kết nối'}
            </Tag>
          </Col>
        </Row>
      </Header>

      <Content style={{ padding: 24, maxWidth: 1320, width: '100%', margin: '0 auto' }}>
        <Space direction="vertical" size={16} style={{ width: '100%' }}>
          {isAlert && (
            <Alert
              type="error"
              showIcon
              banner
              style={{ borderRadius: 12 }}
              message="CẢNH BÁO NHIỆT ĐỘ"
              description={`Nhiệt độ ${dashboard.currentTemperature.toFixed(
                1
              )}°C đã vượt ngưỡng ${dashboard.maxTemperatureThreshold}°C!`}
            />
          )}

          <StatCards data={dashboard} />

          <Row gutter={[16, 16]}>
            <Col xs={24} lg={16}>
              <Space direction="vertical" size={16} style={{ width: '100%' }}>
                <TempChart refreshKey={refreshKey} />
                <HistoryTable rows={history} />
              </Space>
            </Col>
            <Col xs={24} lg={8}>
              <Space direction="vertical" size={16} style={{ width: '100%' }}>
                <AlertSettings onSaved={refresh} />
                <AlertLogs logs={logs} />
              </Space>
            </Col>
          </Row>

          <div className="app-footer">
            Đồ án IoT © 2026 · Tự động cập nhật mỗi {REFRESH_MS / 1000} giây
          </div>
        </Space>
      </Content>
    </Layout>
  );
}
