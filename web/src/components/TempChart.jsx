import { useEffect, useState } from 'react';
import { Card, Segmented, Spin, Empty } from 'antd';
import { LineChartOutlined } from '@ant-design/icons';
import {
  ResponsiveContainer,
  AreaChart,
  Area,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
} from 'recharts';
import { api } from '../api.js';

const PERIODS = [
  { label: 'Giờ', value: 'hourly' },
  { label: 'Ngày', value: 'daily' },
  { label: 'Tuần', value: 'weekly' },
];

// Biểu đồ nhiệt độ TB/Min/Max theo giờ, ngày hoặc tuần.
export default function TempChart({ refreshKey }) {
  const [period, setPeriod] = useState('hourly');
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let alive = true;
    setLoading(true);
    api
      .getStatistics(period)
      .then((rows) => {
        if (!alive) return;
        setData(
          rows.map((x) => ({
            label: x.label,
            avg: Number(x.avgTemperature.toFixed(1)),
            min: Number(x.minTemperature.toFixed(1)),
            max: Number(x.maxTemperature.toFixed(1)),
          }))
        );
      })
      .catch(() => alive && setData([]))
      .finally(() => alive && setLoading(false));
    return () => {
      alive = false;
    };
  }, [period, refreshKey]);

  return (
    <Card
      title={
        <span className="section-title">
          <LineChartOutlined style={{ color: '#1677ff' }} /> Biểu đồ nhiệt độ
        </span>
      }
      extra={
        <Segmented options={PERIODS} value={period} onChange={setPeriod} />
      }
    >
      <div style={{ height: 320 }}>
        {loading ? (
          <div
            style={{
              height: '100%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <Spin />
          </div>
        ) : data.length === 0 ? (
          <Empty
            description="Chưa có dữ liệu"
            style={{ paddingTop: 90 }}
          />
        ) : (
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={data} margin={{ top: 10, right: 16, left: -8, bottom: 0 }}>
              <defs>
                <linearGradient id="avgFill" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="#1677ff" stopOpacity={0.35} />
                  <stop offset="100%" stopColor="#1677ff" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#eef2f7" />
              <XAxis dataKey="label" tick={{ fontSize: 12, fill: '#64748b' }} />
              <YAxis
                tick={{ fontSize: 12, fill: '#64748b' }}
                unit="°"
                width={44}
              />
              <Tooltip
                contentStyle={{ borderRadius: 10, border: '1px solid #e6ecf5' }}
                formatter={(v, name) => [`${v} °C`, name]}
              />
              <Legend />
              <Area
                type="monotone"
                dataKey="avg"
                name="Trung bình"
                stroke="#1677ff"
                strokeWidth={2.5}
                fill="url(#avgFill)"
              />
              <Line
                type="monotone"
                dataKey="max"
                name="Cao nhất"
                stroke="#ff4d4f"
                strokeWidth={2}
                strokeDasharray="5 4"
                dot={false}
              />
              <Line
                type="monotone"
                dataKey="min"
                name="Thấp nhất"
                stroke="#13c2c2"
                strokeWidth={2}
                strokeDasharray="5 4"
                dot={false}
              />
            </AreaChart>
          </ResponsiveContainer>
        )}
      </div>
    </Card>
  );
}
