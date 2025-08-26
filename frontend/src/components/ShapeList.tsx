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
        <div>
            <h1>Shapes</h1>
            <ul>
                {shapes.map((shape) => (
                    <li key={shape.id}>
                        {shape.name} - ({shape.geometry.type})
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default ShapeList;
