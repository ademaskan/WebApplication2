
export interface Rule {
    id: number;
    name: string;
    description: string;
    geometryType: string;
    shapeType: string;
    relatedGeometryType: string;
    relatedShapeType: string;
    validationType: string;
    buffer: number;
    enabled: boolean;
}

const API_BASE_URL = 'http://localhost:5294/api';

export const getRules = async (): Promise<Rule[]> => {
    const response = await fetch(`${API_BASE_URL}/Rules`);
    if (!response.ok) {
        throw new Error('Failed to fetch rules');
    }
    return response.json();
};

export const addRule = async (rule: Omit<Rule, 'id'>): Promise<Rule> => {
    const response = await fetch(`${API_BASE_URL}/Rules`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(rule),
    });
    if (!response.ok) {
        throw new Error('Failed to add rule');
    }
    return response.json();
};

export const updateRule = async (id: number, rule: Rule): Promise<void> => {
    const response = await fetch(`${API_BASE_URL}/Rules/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(rule),
    });
    if (!response.ok) {
        throw new Error('Failed to update rule');
    }
};

export const deleteRule = async (id: number): Promise<void> => {
    const response = await fetch(`${API_BASE_URL}/Rules/${id}`, {
        method: 'DELETE',
    });
    if (!response.ok) {
        throw new Error('Failed to delete rule');
    }
};
