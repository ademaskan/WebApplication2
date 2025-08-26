import React, { useState, useEffect } from 'react';
import { type Shape } from '../services/shapeService';

interface ShapeListProps {
    shapes: Shape[];
}

const ShapeList: React.FC<ShapeListProps> = ({ shapes }) => {
    if (!shapes) return <div>Loading...</div>;

    return (
        <div style={{ width: '300px', padding: '10px', borderLeft: '1px solid #ccc', overflowY: 'auto', backgroundColor: '#fff', color: '#333' }}>
            <h2 style={{ color: '#0056b3' }}>Shapes</h2>
            <ul style={{ listStyleType: 'none', padding: 0 }}>
                {shapes.map((shape) => (
                    <li key={shape.id} style={{ marginBottom: '15px', border: '1px solid #0056b3', padding: '10px', borderRadius: '5px' }}>
                        <strong>ID:</strong> {shape.id}<br />
                        <strong>Name:</strong> {shape.name}<br />
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default ShapeList;
