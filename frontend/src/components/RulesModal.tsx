
import React, { useState, useEffect } from 'react';
import Popup from './Popup';
import { getRules, addRule, deleteRule, type Rule } from '../services/ruleService';
import './RulesModal.css';

interface RulesModalProps {
    isOpen: boolean;
    onClose: () => void;
}

const RulesModal: React.FC<RulesModalProps> = ({ isOpen, onClose }) => {
    const [rules, setRules] = useState<Rule[]>([]);
    const [newRule, setNewRule] = useState({
        name: '',
        description: '',
        geometryType: 'Point',
        shapeType: 'A',
        relatedGeometryType: 'Point',
        relatedShapeType: 'A',
        validationType: 'CannotIntersect',
        buffer: 0,
        enabled: true,
    });

    const fetchRules = () => {
        getRules()
            .then(setRules)
            .catch(error => console.error('Failed to fetch rules:', error));
    };

    useEffect(() => {
        if (isOpen) {
            fetchRules();
        }
    }, [isOpen]);

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setNewRule(prev => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await addRule(newRule);
            fetchRules(); // Refresh the list
            // Reset form if needed
            setNewRule({
                name: '',
                description: '',
                geometryType: 'Point',
                shapeType: 'A',
                relatedGeometryType: 'Point',
                relatedShapeType: 'A',
                validationType: 'CannotIntersect',
                buffer: 0,
                enabled: true,
            });
        } catch (error) {
            console.error('Failed to add rule:', error);
        }
    };

    const handleDelete = async (id: number) => {
        try {
            await deleteRule(id);
            fetchRules(); // Refresh the list
        } catch (error) {
            console.error('Failed to delete rule:', error);
        }
    };

    return (
        <Popup isOpen={isOpen} onClose={onClose} title="Validation Rules">
            <div>
                <h3>Add New Rule</h3>
                <form onSubmit={handleSubmit} className="add-rule-form">
                    <input name="name" value={newRule.name} onChange={handleInputChange} placeholder="Name" required />
                    <input name="description" value={newRule.description} onChange={handleInputChange} placeholder="Description" />
                    <input type="number" name="buffer" value={newRule.buffer} onChange={handleInputChange} placeholder="Buffer" />
                    <select name="geometryType" value={newRule.geometryType} onChange={handleInputChange}>
                        <option value="Point">Point</option>
                        <option value="LineString">LineString</option>
                        <option value="Polygon">Polygon</option>
                    </select>
                    <select name="shapeType" value={newRule.shapeType} onChange={handleInputChange}>
                        <option value="A">A</option>
                        <option value="B">B</option>
                        <option value="C">C</option>
                    </select>
                    <select name="relatedGeometryType" value={newRule.relatedGeometryType} onChange={handleInputChange}>
                        <option value="Point">Point</option>
                        <option value="LineString">LineString</option>
                        <option value="Polygon">Polygon</option>
                    </select>
                    <select name="relatedShapeType" value={newRule.relatedShapeType} onChange={handleInputChange}>
                        <option value="A">A</option>
                        <option value="B">B</option>
                        <option value="C">C</option>
                    </select>
                    <select name="validationType" value={newRule.validationType} onChange={handleInputChange}>
                        <option value="CannotIntersect">Cannot Intersect</option>
                    </select>
                    <button type="submit">Add Rule</button>
                </form>

                <table className="rules-table">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Description</th>
                            <th>Buffer</th>
                            <th>Enabled</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rules.map(rule => (
                            <tr key={rule.id}>
                                <td>{rule.name}</td>
                                <td>{rule.description}</td>
                                <td>{rule.buffer}</td>
                                <td>{rule.enabled ? 'Yes' : 'No'}</td>
                                <td>
                                    <button onClick={() => handleDelete(rule.id)} className="delete-btn">Delete</button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </Popup>
    );
};

export default RulesModal;
