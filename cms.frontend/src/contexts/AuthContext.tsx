import {
  createContext,
  useContext,
  useState,
  useCallback,
  useMemo,
  useEffect,
  type ReactNode,
} from 'react';

// ── Types ──────────────────────────────────────────────────────────────────

export interface AuthUser {
  id: number;
  name: string;
  email: string;
  avatarUrl?: string;
}

interface AuthState {
  user: AuthUser | null;
  isLoading: boolean;
}

interface AuthContextValue extends AuthState {
  isLoggedIn: boolean;
  login:    (email: string, password: string) => Promise<void>;
  register: (name: string, email: string, password: string) => Promise<void>;
  logout:   () => void;
}

const STORAGE_KEY = 'shopco_auth';

function mockLogin(email: string): AuthUser {
  return {
    id: 1,
    name: email.split('@')[0].replace(/[._]/g, ' ').replace(/\b\w/g, (c) => c.toUpperCase()),
    email,
    avatarUrl: `https://ui-avatars.com/api/?name=${encodeURIComponent(email)}&background=000000&color=ffffff&size=80`,
  };
}

function mockRegister(name: string, email: string): AuthUser {
  return {
    id: Date.now(),
    name,
    email,
    avatarUrl: `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=000000&color=ffffff&size=80`,
  };
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }): JSX.Element {
  const [state, setState] = useState<AuthState>(() => {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (raw) return { user: JSON.parse(raw) as AuthUser, isLoading: false };
    } catch { /* ignore */ }
    return { user: null, isLoading: false };
  });

  useEffect(() => {
    if (state.user) localStorage.setItem(STORAGE_KEY, JSON.stringify(state.user));
    else localStorage.removeItem(STORAGE_KEY);
  }, [state.user]);

  const login = useCallback(async (email: string, password: string): Promise<void> => {
    setState((s) => ({ ...s, isLoading: true }));
    try {
      await new Promise((r) => setTimeout(r, 800));
      if (!email || !password) throw new Error('Email and password are required.');
      const user = mockLogin(email);
      setState({ user, isLoading: false });
    } catch (err) {
      setState((s) => ({ ...s, isLoading: false }));
      throw err;
    }
  }, []);

  const register = useCallback(async (name: string, email: string, password: string): Promise<void> => {
    setState((s) => ({ ...s, isLoading: true }));
    try {
      await new Promise((r) => setTimeout(r, 800));
      if (!name || !email || !password) throw new Error('All fields are required.');
      if (password.length < 6) throw new Error('Password must be at least 6 characters.');
      const user = mockRegister(name, email);
      setState({ user, isLoading: false });
    } catch (err) {
      setState((s) => ({ ...s, isLoading: false }));
      throw err;
    }
  }, []);

  const logout = useCallback((): void => {
    setState({ user: null, isLoading: false });
  }, []);

  const value = useMemo<AuthContextValue>(() => ({
    user: state.user,
    isLoading: state.isLoading,
    isLoggedIn: state.user !== null,
    login, register, logout,
  }), [state.user, state.isLoading, login, register, logout]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside <AuthProvider>');
  return ctx;
}
