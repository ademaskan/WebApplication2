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
        type: string;
        wkt: string;
        imagePath?: string;
    }
    
    export interface AddShape {
        name: string;
        geometry: Geometry;
        type: string;
        image?: File;
    }

    export interface MergeShapesRequest {
        name: string;
        geometry: Geometry;
        deleteIds: number[];
        type: string;
    }

    export interface PagedResult<T> {
        items: T[];
        totalCount: number;
        pageNumber: number;
        pageSize: number;
        totalPages: number;
    }


    export interface ApiResponse<T> {
        success: boolean;
        data?: T;
        message?: string;
    }
    
    const API_BASE_URL = 'http://localhost:5294/api';
    
    export const getShapes = async (pageNumber: number, pageSize: number): Promise<PagedResult<Shape>> => {
        try {
            const response = await fetch(`${API_BASE_URL}/Shape/GetAll?pageNumber=${pageNumber}&pageSize=${pageSize}`);
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            const apiResponse: ApiResponse<PagedResult<Shape>> = await response.json();
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
    
    export const addShape = async (shape: AddShape): Promise<Shape> => {
        try {
            const formData = new FormData();
            formData.append('name', shape.name);
            formData.append('geometry', JSON.stringify(shape.geometry));
            formData.append('type', shape.type);
            if (shape.image) {
                formData.append('image', shape.image);
            }

            const response = await fetch(`${API_BASE_URL}/Shape/Add`, {
                method: 'POST',
                body: formData,
            });

            const apiResponse: ApiResponse<Shape> = await response.json();

            if (!response.ok || !apiResponse.success) {
                throw new Error(apiResponse.message || 'Failed to add shape');
            }

            if (apiResponse.data) {
                return apiResponse.data;
            } else {
                throw new Error('No data returned from API after adding shape');
            }
        } catch (error) {
            console.error('Error adding shape:', error);
            throw error;
        }
    };
    
    export const deleteAllShapes = async (): Promise<void> => {
        try {
            const response = await fetch(`${API_BASE_URL}/Shape/DeleteAll`, {
                method: 'POST',
            });
    
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
    
            const apiResponse: ApiResponse<object> = await response.json();
    
            if (!apiResponse.success) {
                throw new Error(apiResponse.message || 'Failed to delete all shapes');
            }
        } catch (error) {
            console.error('Error deleting all shapes:', error);
            throw error;
        }
    };

    export const deleteShapeById = async (id: number): Promise<void> => {
        try {
            const response = await fetch(`${API_BASE_URL}/Shape/DeleteById/${id}`, {
                method: 'POST',
            });
    
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
    
            const apiResponse: ApiResponse<object> = await response.json();
    
            if (!apiResponse.success) {
                throw new Error(apiResponse.message || `Failed to delete shape with id ${id}`);
            }
        } catch (error) {
            console.error(`Error deleting shape with id ${id}:`, error);
            throw error;
        }
    };

    export const addShapes = async (shapes: AddShape[]): Promise<Shape[]> => {
        try {
            const shapesWithSerializedGeometry = shapes.map(shape => ({
                ...shape,
                geometry: JSON.stringify(shape.geometry)
            }));

            const response = await fetch(`${API_BASE_URL}/Shape/AddRange`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(shapesWithSerializedGeometry),
            });
    
            const apiResponse: ApiResponse<Shape[]> = await response.json();
    
            if (!response.ok || !apiResponse.success) {
                throw new Error(apiResponse.message || 'Failed to add shapes');
            }
    
            if (apiResponse.data) {
                return apiResponse.data;
            } else {
                throw new Error('No data returned from API after adding shapes');
            }
        } catch (error) {
            console.error('Error adding shapes:', error);
            throw error;
        }
    };

    export const mergeShapes = async (request: MergeShapesRequest): Promise<Shape> => {
        try {
            const response = await fetch(`${API_BASE_URL}/Shape/Merge`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(request),
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const apiResponse: ApiResponse<Shape> = await response.json();

            if (apiResponse.success && apiResponse.data) {
                return apiResponse.data;
            } else {
                throw new Error(apiResponse.message || 'Failed to merge shapes');
            }
        } catch (error) {
            console.error('Error merging shapes:', error);
            throw error;
        }
    }

    export const createTestData = async (count: number): Promise<void> => {
        try {
            const response = await fetch(`${API_BASE_URL}/Shape/CreateTestData/${count}`, {
                method: 'POST',
            });
    
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
    
            const apiResponse: ApiResponse<object> = await response.json();
    
            if (!apiResponse.success) {
                throw new Error(apiResponse.message || 'Failed to create test data');
            }
        } catch (error) {
            console.error('Error creating test data:', error);
            throw error;
        }
    };