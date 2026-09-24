export interface CompanySettings {
  settingId: number;
  companyName: string;
  address?: string;
  city?: string;
  state?: string;
  country: string;
  postalCode?: string;
  phone?: string;
  email?: string;
  website?: string;
  taxNumber?: string;
  logoUrl?: string;
  defaultCurrency: string;
  termsAndConditions?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankIfsc?: string;
}

export interface UpdateCompanySettingsRequest {
  companyName: string;
  address?: string;
  city?: string;
  state?: string;
  country: string;
  postalCode?: string;
  phone?: string;
  email?: string;
  website?: string;
  taxNumber?: string;
  logoUrl?: string;
  defaultCurrency: string;
  termsAndConditions?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankIfsc?: string;
}
