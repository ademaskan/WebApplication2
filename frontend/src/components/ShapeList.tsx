import React from 'react';
import { type Shape, type Geometry } from '../services/shapeService';

interface ShapeListProps {
    shapes: Shape[];
    onJumpToShape: (geometry: Geometry) => void;
    onClose: () => void;
}

const ShapeList: React.FC<ShapeListProps> = ({ shapes, onJumpToShape, onClose }) => {
    if (!shapes) return <div>Loading...</div>;

    return (
        <div style={{ width: '300px', padding: '10px', borderLeft: '1px solid #ccc', overflowY: 'auto', backgroundColor: '#fff', color: '#333' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <h2 style={{ color: '#0056b3', margin: 0 }}>Geometries</h2>
                <button onClick={onClose} style={{ background: 'none', border: 'none', fontSize: '20px', cursor: 'pointer', color: '#0056b3' }}>&times;</button>
            </div>
            <ul style={{ listStyleType: 'none', padding: 0, marginTop: '10px' }}>
                {shapes.map((shape) => (
                    <li key={shape.id} style={{ marginBottom: '15px', border: '1px solid #0056b3', padding: '10px', borderRadius: '5px' }}>
                        <strong>ID:</strong> {shape.id}<br />
                        <strong>Name:</strong> {shape.name}<br />
                        <strong>Type:</strong> {shape.type}<br />
                        <button onClick={() => onJumpToShape(shape.geometry)} style={{ marginTop: '5px', backgroundColor: '#007bff', color: 'white', border: 'none', padding: '5px 10px', borderRadius: '3px', cursor: 'pointer' }}>
                            Go to
                        </button>
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default ShapeList;
