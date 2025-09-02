import React from 'react';

interface ConfirmationModalProps {
    isOpen: boolean;
    onClose: () => void;
    onConfirm: () => void;
    title: string;
    message: string;
}

const ConfirmationModal: React.FC<ConfirmationModalProps> = ({ isOpen, onClose, onConfirm, title, message }) => {
    if (!isOpen) {
        return null;
    }

    const handleConfirm = () => {
        onConfirm();
        onClose();
    };

    return (
        <div style={modalOverlayStyle}>
            <div style={modalContentStyle}>
                <h2 style={{ color: '#0056b3' }}>{title}</h2>
                <p>{message}</p>
                <div style={{ textAlign: 'right', marginTop: '20px' }}>
                    <button onClick={handleConfirm} style={{ backgroundColor: '#dc3545', color: 'white', border: 'none', padding: '10px 15px', borderRadius: '5px', cursor: 'pointer' }}>Confirm</button>
                    <button onClick={onClose} style={{ marginLeft: '10px', backgroundColor: '#6c757d', color: 'white', border: 'none', padding: '10px 15px', borderRadius: '5px', cursor: 'pointer' }}>Cancel</button>
                </div>
            </div>
        </div>
    );
};

const modalOverlayStyle: React.CSSProperties = {
    position: 'fixed',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    zIndex: 2000,
};

const modalContentStyle: React.CSSProperties = {
    backgroundColor: 'white',
    padding: '20px',
    borderRadius: '5px',
    width: '400px',
    boxShadow: '0 4px 8px rgba(0,0,0,0.1)',
};

export default ConfirmationModal;
