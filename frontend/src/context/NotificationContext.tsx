import React, { createContext, useContext, type ReactNode } from 'react';
import toast, { type ToastOptions } from 'react-hot-toast';

type NotificationType = 'success' | 'error' | 'info' | 'warning';

interface NotificationContextType {
  showNotification: (message: string, type: NotificationType) => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export const useNotifications = () => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotifications must be used within a NotificationProvider');
  }
  return context;
};

interface NotificationProviderProps {
  children: ReactNode;
}

export const NotificationProvider: React.FC<NotificationProviderProps> = ({ children }) => {
  const showNotification = (message: string, type: NotificationType) => {
    const options: ToastOptions = {
      duration: 4000,
      position: 'top-right',
    };

    switch (type) {
      case 'success':
        toast.success(message, options);
        break;
      case 'error':
        toast.error(message, options);
        break;
      case 'info':
        toast(message, { ...options, icon: 'ℹ️' });
        break;
      case 'warning':
        toast(message, { ...options, icon: '⚠️' });
        break;
      default:
        toast(message, options);
        break;
    }
  };

  return (
    <NotificationContext.Provider value={{ showNotification }}>
      {children}
    </NotificationContext.Provider>
  );
};
