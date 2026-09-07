export interface TelemetryRecord {
    vehicleId: string;
    timestamp: string;
    latitude: number;
    longitude: number;
    speedKph: number;
    engineTemperatureC: number;
    faultCode?: string;
}

export interface VehicleSummary {
    vehicleId: string;
    lastSeen: string;
    latestSpeed: number;
    latestTemp: number;
}

export class VirtualFleetClient {
    private baseUrl: string;

    constructor(baseUrl: string = "http://localhost:5000") {
        this.baseUrl = baseUrl;
    }

    // Fetch summary of all active fleet vehicles
    async getActiveVehicles(): Promise<VehicleSummary[]> {
        const response = await fetch(`${this.baseUrl}/api/vehicles`);
        if (!response.ok) {
            throw new Error(`Failed to fetch vehicles: ${response.statusText}`);
        }
        return response.json();
    }

    // Fetch chronological tracking history for a specific vehicle
    async getVehicleHistory(vehicleId: string, limit: number = 20): Promise<TelemetryRecord[]>{
        const response = await fetch(`${this.baseUrl}/api/vehicles/${vehicleId}/history?limit=${limit}`);
        if (!response.ok) {
            throw new Error(`Failed to fetch history for ${vehicleId}: ${response.statusText}`);
        }
        return response.json();
    }
}