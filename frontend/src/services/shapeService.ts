    // Define the types for your data. This should match your backend entities.
    // Note: The Geometry will likely come in as a GeoJSON object.
    
    export interface Geometry {
        type: string;
        coordinates: any[];
    }
    
    export interface Shape {
        id: number;
        name: string;
        geometry: Geometry;
    }
    
    // This is based on the ApiResponse<T> from your backend
    export interface ApiResponse<T> {
        success: boolean;
        data?: T;
        message?: string;
    }
    
    const API_BASE_URL = 'http://localhost:5294/api'; // Use your backend's URL
    
    export const getShapes = async (): Promise<Shape[]> => {
        try {
            const response = await fetch(`${API_BASE_URL}/Shape/GetAll`);
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            const apiResponse: ApiResponse<Shape[]> = await response.json();
            if (apiResponse.success && apiResponse.data) {
                return apiResponse.data;
            } else {
                throw new Error(apiResponse.message || 'Failed to fetch shapes');
            }
        } catch (error) {
            console.error('Error fetching shapes:', error);
            throw error;
        }
    };