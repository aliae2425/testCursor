const FORGE_CLIENT_ID = process.env.REACT_APP_FORGE_CLIENT_ID;
const FORGE_CLIENT_SECRET = process.env.REACT_APP_FORGE_CLIENT_SECRET;

export async function getAccessToken() {
  const url = 'https://developer.api.autodesk.com/authentication/v1/authenticate';
  const headers = {
    'Content-Type': 'application/x-www-form-urlencoded',
  };
  const body = new URLSearchParams({
    client_id: FORGE_CLIENT_ID,
    client_secret: FORGE_CLIENT_SECRET,
    grant_type: 'client_credentials',
    scope: 'data:read viewables:read',
  });

  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: headers,
      body: body,
    });
    const data = await response.json();
    return data.access_token;
  } catch (error) {
    console.error('Erreur lors de l\'authentification:', error);
    throw error;
  }
}

export async function uploadModel(file) {
  const accessToken = await getAccessToken();
  
  // 1. Créer un bucket si nécessaire
  const bucketKey = 'teapot_preview_bucket';
  await createBucket(accessToken, bucketKey);
  
  // 2. Upload le fichier
  const objectKey = `${Date.now()}_${file.name}`;
  const uploadResponse = await uploadFile(accessToken, bucketKey, objectKey, file);
  
  // 3. Traduire le fichier
  const urn = btoa(uploadResponse.objectId);
  await translateFile(accessToken, urn);
  
  return urn;
}

async function createBucket(accessToken, bucketKey) {
  const url = 'https://developer.api.autodesk.com/oss/v2/buckets';
  const headers = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${accessToken}`,
  };
  const body = {
    bucketKey,
    policyKey: 'transient', // Le bucket sera supprimé après 24h
  };

  try {
    await fetch(url, {
      method: 'POST',
      headers: headers,
      body: JSON.stringify(body),
    });
  } catch (error) {
    if (error.response?.status !== 409) { // Ignorer l'erreur si le bucket existe déjà
      throw error;
    }
  }
}

async function uploadFile(accessToken, bucketKey, objectKey, file) {
  const url = `https://developer.api.autodesk.com/oss/v2/buckets/${bucketKey}/objects/${objectKey}`;
  const headers = {
    'Content-Type': 'application/octet-stream',
    'Authorization': `Bearer ${accessToken}`,
  };

  const response = await fetch(url, {
    method: 'PUT',
    headers: headers,
    body: file,
  });
  return response.json();
}

async function translateFile(accessToken, urn) {
  const url = 'https://developer.api.autodesk.com/modelderivative/v2/designdata/job';
  const headers = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${accessToken}`,
  };
  const body = {
    input: {
      urn,
    },
    output: {
      formats: [
        {
          type: 'svf',
          views: ['2d', '3d'],
        },
      ],
    },
  };

  await fetch(url, {
    method: 'POST',
    headers: headers,
    body: JSON.stringify(body),
  });
} 