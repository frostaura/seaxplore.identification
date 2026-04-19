export interface AttributeValue {
  attributeId: number;
  attributeName: string;
  value: string;
}

export interface Species {
  id: number;
  scientificName: string;
  commonName: string;
  description: string;
  imageUrl?: string;
  categoryId: number;
  categoryName: string;
  attributes: AttributeValue[];
}

export interface SearchResult {
  species: Species;
  similarityScore: number;
}

export interface Category {
  id: number;
  name: string;
  description: string;
}

export interface MarineAttribute {
  id: number;
  name: string;
  description: string;
  dataType: string;
}

export interface AdminLoginResponse {
  token: string;
  username: string;
}
