import { Row, Col, Card, Statistic } from 'antd';
import {
  FireOutlined,
  CloudOutlined,
  ArrowUpOutlined,
  ArrowDownOutlined,
} from '@ant-design/icons';

const fmt = (v) => (v == null || Number.isNaN(v) ? '--' : v.toFixed(1));

// 4 thẻ số liệu tổng quan ở đầu trang.
export default function StatCards({ data }) {
  const isAlert = !!data?.isAlert;

  const cards = [
    {
      title: 'Nhiệt độ hiện tại',
      value: fmt(data?.currentTemperature),
      suffix: '°C',
      color: isAlert ? '#ff4d4f' : '#fa541c',
      icon: <FireOutlined />,
      alert: isAlert,
    },
    {
      title: 'Độ ẩm hiện tại',
      value: fmt(data?.currentHumidity),
      suffix: '%',
      color: '#1677ff',
      icon: <CloudOutlined />,
    },
    {
      title: 'Cao nhất hôm nay',
      value: fmt(data?.maxTemperatureToday),
      suffix: '°C',
      color: '#fa8c16',
      icon: <ArrowUpOutlined />,
    },
    {
      title: 'Thấp nhất hôm nay',
      value: fmt(data?.minTemperatureToday),
      suffix: '°C',
      color: '#13c2c2',
      icon: <ArrowDownOutlined />,
    },
  ];

  return (
    <Row gutter={[16, 16]}>
      {cards.map((c) => (
        <Col xs={12} md={6} key={c.title}>
          <Card
            className={`stat-card${c.alert ? ' is-alert' : ''}`}
            styles={{ body: { padding: 20 } }}
          >
            <span className="stat-icon" style={{ color: c.color }}>
              {c.icon}
            </span>
            <Statistic
              title={c.title}
              value={c.value}
              suffix={c.suffix}
              valueStyle={{ color: c.color, fontWeight: 700 }}
            />
          </Card>
        </Col>
      ))}
    </Row>
  );
}
