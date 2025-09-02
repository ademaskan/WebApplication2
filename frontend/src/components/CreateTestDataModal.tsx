import React, { useState } from 'react';

interface CreateTestDataModalProps {
    isOpen: boolean;
    onClose: () => void;
    onGenerate: (count: number) => void;
}

const CreateTestDataModal: React.FC<CreateTestDataModalProps> = ({ isOpen, onClose, onGenerate }) => {
    const [count, setCount] = useState(10);

    if (!isOpen) {
        return null;
    }

    const handleGenerate = () => {
        onGenerate(count);
    };

    return (
        <div style={modalOverlayStyle}>
            <div style={modalContentStyle}>
                <h2 style={{ color: '#0056b3' }}>Create Test Data</h2>
                <p>How many shapes would you like to create?</p>
                <input
                    type="number"
                    value={count}
                    onChange={(e) => setCount(parseInt(e.target.value, 10))}
                    style={{ width: '100%', padding: '8px', marginBottom: '20px', border: '1px solid #ccc', borderRadius: '3px' }}
                    min="1"
                />
                <button onClick={handleGenerate} style={{ backgroundColor: '#007bff', color: 'white', border: 'none', padding: '10px 15px', borderRadius: '5px', cursor: 'pointer' }}>Generate</button>
                <button onClick={onClose} style={{ marginLeft: '10px', backgroundColor: '#6c757d', color: 'white', border: 'none', padding: '10px 15px', borderRadius: '5px', cursor: 'pointer' }}>Cancel</button>
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
    zIndex: 1000,
};

const modalContentStyle: React.CSSProperties = {
    backgroundColor: 'white',
    padding: '20px',
    borderRadius: '5px',
    width: '400px',
    boxShadow: '0 4px 8px rgba(0,0,0,0.1)',
};

export default CreateTestDataModal;
