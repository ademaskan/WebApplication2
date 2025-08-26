import React, { useState, useEffect } from 'react';
import { type Shape } from '../services/shapeService';

interface DeleteShapeModalProps {
    isOpen: boolean;
    onClose: () => void;
    shapes: Shape[];
    onDelete: (id: number) => void;
}

const DeleteShapeModal: React.FC<DeleteShapeModalProps> = ({ isOpen, onClose, shapes, onDelete }) => {
    const [selectedShapeId, setSelectedShapeId] = useState<string>(shapes[0]?.id.toString() || '');

    useEffect(() => {
        if (isOpen && shapes.length > 0) {
            setSelectedShapeId(shapes[0].id.toString());
        } else if (isOpen && shapes.length === 0) {
            setSelectedShapeId('');
        }
    }, [isOpen, shapes]);

    if (!isOpen) {
        return null;
    }

    const handleDelete = () => {
        if (selectedShapeId) {
            onDelete(Number(selectedShapeId));
            onClose();
        }
    };

    return (
        <div style={modalOverlayStyle}>
            <div style={modalContentStyle}>
                <h2 style={{ color: '#0056b3' }}>Delete a Shape</h2>
                {shapes.length > 0 ? (
                    <>
                        <select
                            value={selectedShapeId}
                            onChange={(e) => setSelectedShapeId(e.target.value)}
                            style={{ width: '100%', padding: '8px', marginBottom: '20px', border: '1px solid #ccc', borderRadius: '3px' }}
                        >
                            {shapes.map(shape => (
                                <option key={shape.id} value={shape.id}>
                                    {shape.name} (ID: {shape.id})
                                </option>
                            ))}
                        </select>
                        <button onClick={handleDelete} disabled={!selectedShapeId} style={{ backgroundColor: '#dc3545', color: 'white', border: 'none', padding: '10px 15px', borderRadius: '5px', cursor: 'pointer' }}>Delete</button>
                    </>
                ) : (
                    <p>There are no shapes to delete.</p>
                )}
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

export default DeleteShapeModal;
