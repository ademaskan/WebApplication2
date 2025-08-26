import React, { useState, useEffect } from 'react';
import { type Shape, getShapes } from '../services/shapeService';

const ShapeList: React.FC = () => {
    const [shapes, setShapes] = useState<Shape[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchShapes = async () => {
            try {
                const data = await getShapes();
                setShapes(data);
            } catch (err) {
                setError('Failed to load shapes.');
            } finally {
                setLoading(false);
            }
        };

        fetchShapes();
    }, []);

    if (loading) return <div>Loading...</div>;
    if (error) return <div>{error}</div>;

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
