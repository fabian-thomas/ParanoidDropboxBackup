using System.Threading.Tasks;
using Dropbox.Api.Files;

namespace ParanoidDropboxBackup.Dropbox
{
    public interface IContentVisitor
    {
        public Task Visit(FileMetadata file);
        public Task Visit(FolderMetadata folder);
        public Task Visit(DeletedMetadata deleted);
    }
}