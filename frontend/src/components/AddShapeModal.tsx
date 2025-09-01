import React, { useState } from 'react';

interface AddShapeModalProps {
    isOpen: boolean;
    onClose: () => void;
    onStartDrawing: (name: string, geometryType: 'Point' | 'LineString' | 'Polygon', type: 'A' | 'B' | 'C', image?: File) => void;
}

const AddShapeModal: React.FC<AddShapeModalProps> = ({ isOpen, onClose, onStartDrawing }) => {
    const [name, setName] = useState('');
    const [geometryType, setGeometryType] = useState<'Point' | 'LineString' | 'Polygon'>('Point');
    const [type, setType] = useState<'A' | 'B' | 'C'>('A');
    const [image, setImage] = useState<File | undefined>(undefined);

    if (!isOpen) {
        return null;
    }

    const handleStartDrawing = () => {
        onStartDrawing(name, geometryType, type, image);
        onClose();
    };

    const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            setImage(e.target.files[0]);
        }
    };

    return (
        <div style={modalOverlayStyle}>
            <div style={modalContentStyle}>
                <h2 style={{ color: '#0056b3' }}>Add New Shape</h2>
                <input
                    type="text"
                    placeholder="Shape Name"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    style={{ width: '95%', padding: '8px', marginBottom: '10px', border: '1px solid #ccc', borderRadius: '3px' }}
                />
                <select
                    value={geometryType}
                    onChange={(e) => setGeometryType(e.target.value as any)}
                    style={{ width: '100%', padding: '8px', marginBottom: '20px', border: '1px solid #ccc', borderRadius: '3px' }}
                >
                    <option value="Point">Point</option>
                    <option value="LineString">LineString</option>
                    <option value="Polygon">Polygon</option>
                </select>
                <select
                    value={type}
                    onChange={(e) => setType(e.target.value as any)}
                    style={{ width: '100%', padding: '8px', marginBottom: '20px', border: '1px solid #ccc', borderRadius: '3px' }}
                >
                    <option value="A">A</option>
                    <option value="B">B</option>
                    <option value="C">C</option>
                </select>
                {geometryType === 'Point' && (
                    <input
                        type="file"
                        accept="image/*"
                        onChange={handleImageChange}
                        style={{ width: '95%', padding: '8px', marginBottom: '10px', border: '1px solid #ccc', borderRadius: '3px' }}
                    />
                )}
                <button onClick={handleStartDrawing} style={{ backgroundColor: '#0056b3', color: 'white', border: 'none', padding: '10px 15px', borderRadius: '5px', cursor: 'pointer' }}>Start Drawing</button>
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
    zIndex: 1000, // Ensure it's on top
};

const modalContentStyle: React.CSSProperties = {
    backgroundColor: 'white',
    padding: '20px',
    borderRadius: '5px',
    width: '300px',
    boxShadow: '0 4px 8px rgba(0,0,0,0.1)',
};

export default AddShapeModal;
