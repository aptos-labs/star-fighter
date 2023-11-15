module star_fighter::star_fighter {
    use aptos_std::smart_table::{Self, SmartTable};
    use std::error;
    use std::signer;
    use std::vector;

    /// The account is not authorized to update the resources.
    const ENOT_AUTHORIZED: u64 = 1;

    struct UserStats has key, store, copy {
        games_played: u64,
        best_survival_time_ms: u64,
    }

    struct GlobalState has key {
        users_stats: SmartTable<address, UserStats>,
    }

    struct UserStatsView has drop {
        games_played: u64,
        best_survival_time_ms: u64,
    }

    struct AllUsersStatsViewItem has drop {
        addr: address,
        games_played: u64,
        best_survival_time_ms: u64,
    }

    fun init_module(admin: &signer) {
        move_to(admin, GlobalState {
            users_stats: smart_table::new(),
        })
    }

    entry fun save_game_session(
        admin: &signer,
        user_addr: address,
        survival_time_ms: u64,
    ) acquires GlobalState {
        assert!(signer::address_of(admin) == @star_fighter, error::permission_denied(ENOT_AUTHORIZED));

        let users_stats = &mut borrow_global_mut<GlobalState>(@star_fighter).users_stats;
        if (!smart_table::contains(users_stats, user_addr)) {
            smart_table::add(users_stats, user_addr, UserStats {
                games_played: 1,
                best_survival_time_ms: survival_time_ms
            });
        } else {
            let user_stats = smart_table::borrow_mut(users_stats, user_addr);
            user_stats.games_played = user_stats.games_played + 1;
            if (survival_time_ms > user_stats.best_survival_time_ms) {
                user_stats.best_survival_time_ms = survival_time_ms;
            }
        }
    }

    #[view]
    public fun get_user_stats(
        user: address,
    ): UserStatsView acquires GlobalState {
        let users_stats = &borrow_global<GlobalState>(@star_fighter).users_stats;
        if (!smart_table::contains(users_stats, user)) {
            UserStatsView {
                games_played: 0,
                best_survival_time_ms: 0
            }
        } else {
            let user_stats = smart_table::borrow(users_stats, user);
            UserStatsView {
                games_played: user_stats.games_played,
                best_survival_time_ms: user_stats.best_survival_time_ms
            }
        }
    }

    #[view]
    public fun get_all_user_stats(): vector<AllUsersStatsViewItem> acquires GlobalState {
        let users_stats = &borrow_global<GlobalState>(@star_fighter).users_stats;
        let result = vector::empty<AllUsersStatsViewItem>();

        smart_table::for_each_ref<address, UserStats>(
            users_stats,
            |key, value| {
                let bound_value: &UserStats = value;
                vector::push_back(&mut result, AllUsersStatsViewItem {
                    addr: *key,
                    games_played: value.games_played,
                    best_survival_time_ms: bound_value.best_survival_time_ms,
                });
            }
        );

        result
    }

    #[test(admin = @0x123, user = @0x2)]
    public fun test_module(
        admin: signer,
        user: signer,
    ) acquires GlobalState {
        use std::debug::print;
        use aptos_framework::account;

        let admin_addr = signer::address_of(&admin);
        let user_addr = signer::address_of(&user);

        account::create_account_for_test(admin_addr);
        account::create_account_for_test(user_addr);

        init_module(&admin);
   
        let users_stats = &borrow_global<GlobalState>(@star_fighter).users_stats;
        assert!(!smart_table::contains(users_stats, user_addr), 1);
      
        save_game_session(&admin, user_addr, 100);

        let users_stats = &borrow_global<GlobalState>(@star_fighter).users_stats;
        assert!(smart_table::contains(users_stats, user_addr), 2);

        let user_stats = get_user_stats(user_addr);
        print(&user_stats);
    }
}