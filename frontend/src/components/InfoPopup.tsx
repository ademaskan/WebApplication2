import React, { useState, useEffect } from 'react';
import './Popup.css';
import { type Shape } from '../services/shapeService';
import placeholderImage from '../assets/placeholder.png';


interface InfoPopupProps {
    shape: Shape | null;
    containedShapes: Shape[];
    onClose: () => void;
    onUpdate: (id: number, newName: string) => void;
    onDelete: (id: number) => void;
    onEditModeChange: (isEditing: boolean) => void;
    position: { x: number; y: number } | null;
}

const InfoPopup: React.FC<InfoPopupProps> = ({ shape, containedShapes, onClose, onUpdate, onDelete, onEditModeChange, position }) => {
    const [isEditing, setIsEditing] = useState(false);
    const [newName, setNewName] = useState('');

    useEffect(() => {
        if (shape) {
            setNewName(shape.name);
        }
    }, [shape]);

    useEffect(() => {
        onEditModeChange(isEditing);
    }, [isEditing, onEditModeChange]);

    if (!shape || !position) {
        return null;
    }

    const handleUpdateClick = () => {
        setIsEditing(true);
    };

    const handleCancelClick = () => {
        setIsEditing(false);
        setNewName(shape.name);
    };

    const handleSaveClick = () => {
        onUpdate(shape.id, newName);
        setIsEditing(false);
    };

    const style: React.CSSProperties = {
        position: 'absolute',
        left: `${position.x}px`,
        top: `${position.y}px`,
        transform: 'translate(-50%, calc(-100% - 10px))'
    };

    return (
        <div className="info-popup-content" style={style}>
            <button onClick={onClose} className="info-popup-close-btn">&times;</button>
            <div className="info-popup-header">Shape Details</div>
            <div className="info-popup-body">
                {shape.imagePath && shape.geometry.type === 'Point' && (
                    <img
                        src={`http://localhost:5294/${shape.imagePath.replace(/\\/g, '/')}`}
                        alt={shape.name}
                        style={{ maxWidth: '200px', maxHeight: '200px', height: 'auto', marginBottom: '10px', objectFit: 'cover' }}
                        onError={(e) => {
                            const target = e.target as HTMLImageElement;
                            target.onerror = null; 
                            target.src = placeholderImage;
                        }}
                    />
                )}
                <p><strong>ID:</strong> {shape.id}</p>
                <p><strong>Name:</strong> {isEditing ? <input type="text" value={newName} onChange={(e) => setNewName(e.target.value)} /> : shape.name}</p>
                <p><strong>Type:</strong> {shape.type}</p>
                <p><strong>WKT:</strong> <span className="wkt-text">{shape.wkt}</span></p>
                {containedShapes.length > 0 && (
                    <div className="contained-shapes">
                        <strong>Contained Shapes:</strong>
                        <ul>
                            {containedShapes.map(cs => (
                                <li key={cs.id}>{cs.name} ({cs.geometry.type})</li>
                            ))}
                        </ul>
                    </div>
                )}
                <div className="info-popup-actions">
                    {isEditing ? (
                        <>
                            <button onClick={handleSaveClick}>Save</button>
                            <button onClick={handleCancelClick}>Cancel</button>
                        </>
                    ) : (
                        <>
                            <button onClick={handleUpdateClick}>Update</button>
                            <button onClick={() => onDelete(shape.id)}>Delete</button>
                        </>
                    )}
                </div>
            </div>
        </div>
    );
};

export default InfoPopup;
