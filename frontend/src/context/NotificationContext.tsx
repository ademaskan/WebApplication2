import React, { createContext, useContext, type ReactNode } from 'react';
import toast, { type ToastOptions } from 'react-hot-toast';

// Bildirim türleri (başarı, hata, bilgi, uyarı)
type NotificationType = 'success' | 'error' | 'info' | 'warning';

interface NotificationContextType {
  showNotification: (message: string, type: NotificationType) => void;
}

// Bildirim sisteminin ana context'i oluşturuluyor.
const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

// Bu custom hook, bileşenlerin bildirim context'ine kolayca erişmesini sağlar.
export const useNotifications = () => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotifications hook\'u bir NotificationProvider içinde kullanılmalıdır');
  }
  return context;
};

interface NotificationProviderProps {
  children: ReactNode;
}

// Provider bileşeni, uygulamayı sararak bildirim fonksiyonunu her yerde kullanılabilir hale getirir.
export const NotificationProvider: React.FC<NotificationProviderProps> = ({ children }) => {
  // Bildirimleri göstermek için kullanılan ana fonksiyon.
  const showNotification = (message: string, type: NotificationType) => {
    // Tüm bildirimler için ortak ayarlar.
    const options: ToastOptions = {
      duration: 4000,
      position: 'top-right',
    };

    // Gelen bildirim türüne göre uygun toast fonksiyonunu çağırır.
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
    // showNotification fonksiyonunu alt bileşenlerin kullanımına sunar.
    <NotificationContext.Provider value={{ showNotification }}>
      {children}
    </NotificationContext.Provider>
  );
};
