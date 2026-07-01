import { useEffect, useState } from 'react';
import {
  Card,
  Form,
  InputNumber,
  Switch,
  Button,
  App as AntApp,
} from 'antd';
import { SettingOutlined, SaveOutlined } from '@ant-design/icons';
import { api } from '../api.js';

// Cấu hình ngưỡng nhiệt độ tối đa và bật/tắt cảnh báo.
export default function AlertSettings({ onSaved }) {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const { message } = AntApp.useApp();

  useEffect(() => {
    api
      .getAlertSetting()
      .then((s) =>
        form.setFieldsValue({
          maxTemperature: s.maxTemperature,
          isActive: s.isActive,
        })
      )
      .catch(() => {});
  }, [form]);

  const onFinish = async (values) => {
    setLoading(true);
    try {
      await api.updateAlertSetting(values);
      message.success('Đã lưu cấu hình cảnh báo!');
      onSaved?.();
    } catch {
      message.error('Lưu cấu hình thất bại.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card
      title={
        <span className="section-title">
          <SettingOutlined style={{ color: '#1677ff' }} /> Cấu hình cảnh báo
        </span>
      }
    >
      <Form
        form={form}
        layout="vertical"
        onFinish={onFinish}
        initialValues={{ maxTemperature: 35, isActive: true }}
      >
        <Form.Item
          label="Ngưỡng nhiệt độ tối đa (°C)"
          name="maxTemperature"
          rules={[{ required: true, message: 'Nhập ngưỡng nhiệt độ' }]}
        >
          <InputNumber
            min={0}
            max={100}
            step={0.5}
            addonAfter="°C"
            style={{ width: '100%' }}
          />
        </Form.Item>
        <Form.Item label="Bật cảnh báo" name="isActive" valuePropName="checked">
          <Switch checkedChildren="Bật" unCheckedChildren="Tắt" />
        </Form.Item>
        <Button
          type="primary"
          htmlType="submit"
          icon={<SaveOutlined />}
          loading={loading}
          block
        >
          Lưu cấu hình
        </Button>
      </Form>
    </Card>
  );
}
