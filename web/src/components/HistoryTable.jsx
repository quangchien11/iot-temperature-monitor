import { Card, Table, Tag } from 'antd';
import { HistoryOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';

const columns = [
  { title: '#', dataIndex: 'id', width: 70 },
  {
    title: 'Nhiệt độ',
    dataIndex: 'temperature',
    render: (v) => <b>{v.toFixed(1)} °C</b>,
  },
  {
    title: 'Độ ẩm',
    dataIndex: 'humidity',
    render: (v) => `${v.toFixed(1)} %`,
  },
  {
    title: 'Thời gian',
    dataIndex: 'createdAt',
    render: (v) => dayjs(v).format('HH:mm:ss DD/MM/YYYY'),
  },
  {
    title: 'Trạng thái',
    dataIndex: 'isAlert',
    align: 'center',
    render: (v) =>
      v ? (
        <Tag color="error">Cảnh báo</Tag>
      ) : (
        <Tag color="success">Bình thường</Tag>
      ),
  },
];

// Bảng lịch sử các bản ghi gần nhất.
export default function HistoryTable({ rows }) {
  return (
    <Card
      title={
        <span className="section-title">
          <HistoryOutlined style={{ color: '#1677ff' }} /> Lịch sử gần nhất
        </span>
      }
      styles={{ body: { padding: 0 } }}
    >
      <Table
        rowKey="id"
        size="small"
        columns={columns}
        dataSource={rows}
        pagination={{ pageSize: 8, hideOnSinglePage: true, size: 'small' }}
        scroll={{ x: 460 }}
      />
    </Card>
  );
}
