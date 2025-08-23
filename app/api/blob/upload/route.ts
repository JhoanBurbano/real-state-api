import { handleUpload } from '@vercel/blob/client';
import { NextRequest, NextResponse } from 'next/server';

const MAX_GALLERY_IMAGES = 12;

export async function POST(request: NextRequest) {
  try {
    const { propertyId, kind, index } = await request.json();

    // Validation
    if (!propertyId) {
      return NextResponse.json(
        { error: 'propertyId is required' },
        { status: 400 }
      );
    }

    if (!['cover', 'gallery'].includes(kind)) {
      return NextResponse.json(
        { error: 'kind must be either "cover" or "gallery"' },
        { status: 400 }
      );
    }

    if (kind === 'gallery') {
      if (typeof index !== 'number' || index < 1 || index > MAX_GALLERY_IMAGES) {
        return NextResponse.json(
          { error: `gallery index must be between 1 and ${MAX_GALLERY_IMAGES}` },
          { status: 400 }
        );
      }
    }

    // Compute pathname based on kind and index
    let pathname: string;
    if (kind === 'cover') {
      pathname = `properties/${propertyId}/cover`;
    } else {
      pathname = `properties/${propertyId}/${index}`;
    }

    // Get the upload URL from Vercel Blob
    const { url, pathname: blobPathname, contentType } = await handleUpload({
      pathname,
      clientPayload: { propertyId, kind, index },
      access: 'public',
    });

    return NextResponse.json({
      url,
      pathname: blobPathname,
      contentType,
      propertyId,
      kind,
      index: kind === 'gallery' ? index : undefined,
    });

  } catch (error) {
    console.error('Blob upload error:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}

export async function GET() {
  return NextResponse.json({
    message: 'Vercel Blob upload endpoint',
    maxGalleryImages: MAX_GALLERY_IMAGES,
    usage: {
      cover: 'POST with { propertyId, kind: "cover" }',
      gallery: 'POST with { propertyId, kind: "gallery", index: 1-12 }'
    }
  });
}
