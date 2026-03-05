from locust import HttpUser, task, between


class BilletterieUser(HttpUser):
    """
    Scénario de base pour tester les performances de l'API Billetterie
    en environnement Testing (SQLite: BilleterieSpectacles_Test.db).

    Par défaut, Locust enverra les requêtes vers l'hôte que vous aurez
    configuré dans l'interface web (ex: http://localhost:5293).
    """

    wait_time = between(1, 3)  # temps d'attente aléatoire entre 1 et 3 secondes

    def on_start(self):
        """
        Appelé au démarrage de chaque utilisateur virtuel.
        On récupère un token JWT pour l'utilisateur de test 'test@billetterie.com'.
        """
        login_payload = {
            "email": "test@billetterie.com",
            "password": "TestTest123!",
        }

        with self.client.post("/api/auth/login", json=login_payload, catch_response=True) as response:
            if response.status_code == 200:
                data = response.json()
                token = data.get("token")
                if token:
                    self.client.headers.update({"Authorization": f"Bearer {token}"})
                else:
                    response.failure("Réponse login sans token")
            else:
                response.failure(f"Échec du login: {response.status_code} {response.text}")

    @task(3)
    def list_spectacles(self):
        """
        Récupération de la liste des spectacles.
        """
        self.client.get("/api/spectacles", name="GET /api/spectacles")

    @task(1)
    def get_spectacle_with_performances(self):
        """
        Récupération d'un spectacle avec ses performances.
        On suppose ici qu'un spectacle avec ID 1 existe dans la base de test.
        """
        self.client.get("/api/spectacles/1/performances", name="GET /api/spectacles/{id}/performances")

