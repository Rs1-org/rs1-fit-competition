export function toFormData(obj: Record<string, any>, files?: Record<string, File>): FormData {
  const formData = new FormData();

  Object.entries(obj).forEach(([key, value]) => {
    if (value !== null && value !== undefined) {
      formData.append(key, value.toString());
    }
  });

  if (files) {
    Object.entries(files).forEach(([key, file]) => {
      formData.append(key, file);
    });
  }

  return formData;
}