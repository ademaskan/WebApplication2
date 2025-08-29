import React from 'react';

interface ErrorModalProps {
    isOpen: boolean;
    onClose: () => void;
    message: string;
}

const ErrorModal: React.FC<ErrorModalProps> = ({ isOpen, onClose, message }) => {
    if (!isOpen) {
        return null;
    }

    return (
        <div style={modalOverlayStyle}>
            <div style={modalContentStyle}>
                <h2 style={{ color: '#dc3545' }}>Error</h2>
                <p style={{ margin: '20px 0' }}>{message}</p>
                <button onClick={onClose} style={buttonStyle}>OK</button>
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
    padding: '20px 30px',
    borderRadius: '5px',
    width: '350px',
    boxShadow: '0 4px 8px rgba(0,0,0,0.1)',
    textAlign: 'center',
};

const buttonStyle: React.CSSProperties = {
    backgroundColor: '#0056b3',
    color: 'white',
    border: 'none',
    padding: '10px 20px',
    borderRadius: '5px',
    cursor: 'pointer',
    fontSize: '16px',
    width: '100px'
};

export default ErrorModal;
