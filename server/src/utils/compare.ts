export function compareAscending<T>(lhs: T, rhs: T) {
  if (lhs < rhs) {
    return -1;
  }
  if (lhs > rhs) {
    return 1;
  }
  return 0;
}

export function compareDescending<T>(lhs: T, rhs: T) {
  return compareAscending(rhs, lhs);
}