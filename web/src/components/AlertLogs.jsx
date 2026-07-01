import { Card, List, Empty, Avatar } from 'antd';
import { BellOutlined, WarningFilled } from '@ant-design/icons';
import dayjs from 'dayjs';

// Danh sách nhật ký cảnh báo gần nhất.
export default function AlertLogs({ logs }) {
  return (
    <Card
      title={
        <span className="section-title">
          <BellOutlined style={{ color: '#1677ff' }} /> Nhật ký cảnh báo
        </span>
      }
      styles={{ body: { padding: logs?.length ? '4px 8px' : 24, maxHeight: 320, overflow: 'auto' } }}
    >
      {logs?.length ? (
        <List
          itemLayout="horizontal"
          dataSource={logs}
          renderItem={(item) => (
            <List.Item>
              <List.Item.Meta
                avatar={
                  <Avatar
                    style={{ background: '#fff1f0' }}
                    icon={<WarningFilled style={{ color: '#ff4d4f' }} />}
                  />
                }
                title={<span style={{ fontWeight: 500 }}>{item.message}</span>}
                description={dayjs(item.createdAt).format('HH:mm:ss DD/MM/YYYY')}
              />
            </List.Item>
          )}
        />
      ) : (
        <Empty description="Chưa có cảnh báo nào" image={Empty.PRESENTED_IMAGE_SIMPLE} />
      )}
    </Card>
  );
}
