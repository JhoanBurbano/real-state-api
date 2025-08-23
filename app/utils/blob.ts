import { upload } from '@vercel/blob/client';

export interface BlobUploadOptions {
  propertyId: string;
  kind: 'cover' | 'gallery';
  index?: number;
  file: File;
}

export interface BlobUploadResponse {
  url: string;
  pathname: string;
  contentType: string;
  propertyId: string;
  kind: 'cover' | 'gallery';
  index?: number;
}

export async function uploadPropertyImage(options: BlobUploadOptions): Promise<BlobUploadResponse> {
  const { propertyId, kind, index, file } = options;

  // Validate inputs
  if (!propertyId) {
    throw new Error('propertyId is required');
  }

  if (kind === 'gallery' && (!index || index < 1 || index > 12)) {
    throw new Error('gallery index must be between 1 and 12');
  }

  // Compute pathname
  let pathname: string;
  if (kind === 'cover') {
    pathname = `properties/${propertyId}/cover`;
  } else {
    pathname = `properties/${propertyId}/${index}`;
  }

  // Upload to Vercel Blob
  const { url, pathname: blobPathname, contentType } = await upload(pathname, file, {
    access: 'public',
    handleUploadUrl: '/api/blob/upload',
    clientPayload: { propertyId, kind, index },
  });

  return {
    url,
    pathname: blobPathname,
    contentType,
    propertyId,
    kind,
    index: kind === 'gallery' ? index : undefined,
  };
}

export async function uploadCoverImage(propertyId: string, file: File): Promise<BlobUploadResponse> {
  return uploadPropertyImage({
    propertyId,
    kind: 'cover',
    file,
  });
}

export async function uploadGalleryImage(propertyId: string, index: number, file: File): Promise<BlobUploadResponse> {
  return uploadPropertyImage({
    propertyId,
    kind: 'gallery',
    index,
    file,
  });
}
