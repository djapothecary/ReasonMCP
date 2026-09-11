export const ExternalContextState = {
    pendingPaths: [] as string[],

    addPath: (path: string) => {
        if (!ExternalContextState.pendingPaths.includes(path)) {
            ExternalContextState.pendingPaths.push(path);
        }
    },

    consumePaths: (): string[] => {
        const paths = [...ExternalContextState.pendingPaths];
        ExternalContextState.pendingPaths = []; //  Clear the path(s) after reading
        return paths;
    }
};